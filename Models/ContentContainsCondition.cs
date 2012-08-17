using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Orchard.Rules.Models
{
    public class ContentContainsList
    {
        private int nWordsStart = 1;
        private int nWordsEnd = 3;

        private List<ContentContainsItem> _list;
        public List<ContentContainsItem> List { get { return this._list; } }

        public ContentContainsList(Models.EventContext context)
        {
            _list = new List<ContentContainsItem>();
            for (int i = nWordsStart; i <= nWordsEnd; i++)
            {
                ContentContainsItem item = ContentContainsItem.Load(context, i);

                if (item != null)
                {
                    _list.Add(item);
                }
            }
        }

        public bool CheckOld(string text)
        {
            if (_list != null && _list.Count > 0)
            {
                bool containsPrevious = text.Contains(_list[0].Value);
                if (_list.Count == 1)
                {
                    return containsPrevious;
                }
                else if (_list.Count > 1)
                {
                    bool defaultValue = false;

                    for (int i = 1; i < _list.Count(); i++)
                    {
                        bool containsNext = text.Contains(_list[i].Value);
                        ContentContainsItemNaming.SearchOperators operation = _list[i].Operation;

                        switch (operation)
                        {
                            case ContentContainsItemNaming.SearchOperators.And:
                                if (containsPrevious == false || containsNext == false)
                                {
                                    return false;
                                }

                                defaultValue = true;
                                break;
                            case ContentContainsItemNaming.SearchOperators.Or:
                                if (containsPrevious == true || containsNext == true)
                                {
                                    return true;
                                }

                                defaultValue = false;
                                break;
                        }

                        containsPrevious = containsNext;
                    }

                    return defaultValue;
                }
            }
            return false;
        }

        public void AddText(int ContentPartDefinitionRecordId, string text)
        {
            foreach(ContentContainsItem item in _list.Where(x => x.ContentPartId == ContentPartDefinitionRecordId)) {
                item.Tests.Add(text);
                System.Diagnostics.Debug.WriteLine(ContentPartDefinitionRecordId + " === " + text);
            }
        }

        public bool Check()
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
                        ContentContainsItemNaming.SearchOperators operation = _list[i].Operation;

                        switch (operation)
                        {
                            case ContentContainsItemNaming.SearchOperators.And:
                                if (previousCheck == false || nextCheck == false)
                                {
                                    return false;
                                }

                                defaultValue = true;
                                break;
                            case ContentContainsItemNaming.SearchOperators.Or:
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
    }



    public static class ContentContainsItemNaming
    {
        public enum SearchOperators
        {
            [Display(Name = "And")]
            And,

            [Display(Name = "Or")]
            Or
        }

        public static string GetGroupName(int id)
        {
            return "Word" + id + "Operation";
        }

        public static string GetOperationName(int id)
        {
            return "Word" + id + "Operation";
        }

        public static string GetSourceName(int id)
        {
            return "Word" + id + "Source";
        }

        public static string GetValueName(int id)
        {
            return "Word" + id + "Value";
        }
    }

    public class ContentContainsItem
    {
        private ContentContainsItemNaming.SearchOperators _operation;
        public ContentContainsItemNaming.SearchOperators Operation { get { return _operation; } }

        private int _contentPartId;
        public int ContentPartId { get { return _contentPartId; } }

        private string _value;
        public string Value { get { return _value; } }

        private List<string> _tests;
        public List<string> Tests { get { return _tests; } }

        /// <summary>
        /// Loads the vale
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ContentContainsItem Load(Models.EventContext context, int id)
        {
            string valueName = ContentContainsItemNaming.GetValueName(id);
            if (context.Properties.ContainsKey(valueName))
            {
                string value = context.Properties[valueName];

                if (string.IsNullOrEmpty(value) == false) // If no value is defined then this condition is not important
                {
                    ContentContainsItemNaming.SearchOperators operation = ContentContainsItemNaming.SearchOperators.And; // Default Condition

                    string operationName = ContentContainsItemNaming.GetOperationName(id);
                    if (context.Properties.ContainsKey(operationName))
                    {
                        operation = (ContentContainsItemNaming.SearchOperators)int.Parse(context.Properties[operationName]);
                    }

                    int sourceID = 0;
                    string sourceName = ContentContainsItemNaming.GetSourceName(id);
                    if (context.Properties.ContainsKey(sourceName))
                    {
                        sourceID = int.Parse(context.Properties[sourceName]);
                    }

                    return new ContentContainsItem() { _operation = operation, _contentPartId = sourceID, _value = value, _tests = new List<string>() };
                }
            }

            return null;
        }

        public bool Check()
        {
            foreach (string test in _tests)
            {
                if (test.Contains(_value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}