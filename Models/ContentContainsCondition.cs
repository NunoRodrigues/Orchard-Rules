using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Metadata.Records;
using Orchard.Data;

namespace Orchard.Rules.Models
{
    public class ContentContainsList
    {
        private IRepository<ContentPartDefinitionRecord> _sourcesRepository;

        private int nWordsStart = 1;
        private int nWordsEnd = 3;

        private List<ContentContainsItem> _list;
        public List<ContentContainsItem> List { get { return this._list; } }

        public ContentContainsList(IRepository<ContentPartDefinitionRecord> sourcesRepository, Models.EventContext context)
        {
            _sourcesRepository = sourcesRepository;
            _list = new List<ContentContainsItem>();

            if (context != null)
            {
                for (int i = nWordsStart; i <= nWordsEnd; i++)
                {
                    ContentContainsItem item = ContentContainsItem.Load(context, i);

                    if (item != null)
                    {
                        _list.Add(item);
                    }
                }
            }
        }

        public void AddText(int ContentPartDefinitionRecordId, string text)
        {
            foreach(ContentContainsItem item in _list.Where(x => x.ContentPartId == ContentPartDefinitionRecordId)) {
                item.Tests.Add(text);
                System.Diagnostics.Debug.WriteLine(ContentPartDefinitionRecordId + " === " + text);
            }
        }

        private void Clear()
        {
            foreach (ContentContainsItem item in _list)
            {
                if(item.Tests.Count > 0 ) item.Tests.Clear();
            }
        }

        private void Load(IEnumerable<ContentPart> parts)
        {
            foreach (ContentPart part in parts)
            {
                Type type = part.GetType();

                ContentPartDefinitionRecord source = _sourcesRepository.Table.FirstOrDefault(x => x.Name.Contains(type.Name));
                if (source != null)
                {
                    System.Diagnostics.Debug.WriteLine(source.Id + " ::: " + source.Name);

                    PropertyInfo[] props = type.GetProperties();

                    // Title
                    PropertyInfo title = props.FirstOrDefault(p => p.Name == "Title");
                    if (title != null)
                    {
                        object value = title.GetValue(part, null);
                        AddText(source.Id, value.ToString());
                    }

                    // Body
                    PropertyInfo body = props.FirstOrDefault(p => p.Name == "Body");
                    if (body != null)
                    {
                        object value = body.GetValue(part, null);
                        AddText(source.Id, value.ToString());
                    }

                    // Text
                    PropertyInfo text = props.FirstOrDefault(p => p.Name == "Text");
                    if (text != null)
                    {
                        object value = text.GetValue(part, null);
                        AddText(source.Id, value.ToString());
                    }

                    // TODO : Tags
                    PropertyInfo tags = props.FirstOrDefault(p => p.Name == "CurrentTags");
                    if (tags != null)
                    {
                        /*
                        object value = tags.GetValue(part, null);
                        wordList.AddText(source.Id, value.ToString());
                         */
                    }

                    System.Diagnostics.Debug.WriteLine(" ::: ");
                }
            }
        }

        private bool Check()
        {
            if (_list != null && _list.Count > 0)
            {
                bool previousCheck = _list[0].Check();
                if (_list.Count == 1)
                {
                    return previousCheck;
                }
                else
                {
                    bool defaultValue = false;

                    for (int i = 1; i < _list.Count(); i++)
                    {
                        bool nextCheck = _list[i].Check();
                        ContentContainsItem.SearchOperators operation = _list[i].Operation;

                        switch (operation)
                        {
                            case ContentContainsItem.SearchOperators.And:
                                if (previousCheck == false || nextCheck == false)
                                {
                                    return false;
                                }

                                defaultValue = true;
                                break;
                            case ContentContainsItem.SearchOperators.Or:
                                if (previousCheck == true || nextCheck == true)
                                {
                                    return true;
                                }

                                defaultValue = false;
                                break;
                        }

                        previousCheck = nextCheck;
                    }

                    return defaultValue;
                }
            }

            return false;
        }

        public bool Check(IEnumerable<ContentPart> parts)
        {

            Clear();

            Load(parts);

            return Check();
        }
    }

}