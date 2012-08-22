using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Settings.Metadata.Records;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Rules.Models;
using Orchard.Rules.Services;

namespace Orchard.Rules.Events
{
    public class ContentContainsHanlder : ContentHandler
    {
        public ContentContainsHanlder(IRulesManager rulesManager)
        {

            OnPublished<ContentPart>(
                (context, part) =>
                    rulesManager.TriggerEvent("Content", "Contains",
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));

        }
    }

    public class ContentContainsEvent : IEventProvider
    {
        private static IRepository<ContentPartDefinitionRecord> _sourcesRepository;

        public ContentContainsEvent(IRepository<ContentPartDefinitionRecord> sourcesRepository)
        {
            _sourcesRepository = sourcesRepository;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(Models.DescribeEventContext describe)
        {
            Func<Models.EventContext, bool> condition = Condition;
            Func<Models.EventContext, LocalizedString> display = (Func<Models.EventContext, LocalizedString>)(context => Display(context));

            describe.For("Content", T("Content Items"), T("Content Items")).Element("Contains", T("Content Contains"), T("Content contains text"), condition, display, "SelectContentTypes");
        }

        private static bool Condition(dynamic context) //Models.EventContext
        {
            bool IsContentCorrect = false;
            string contenttypes = context.Properties["contenttypes"];
            var content = context.Tokens["Content"] as IContent;

            if (String.IsNullOrEmpty(contenttypes)) // "" means 'any'
            {
                IsContentCorrect = true;
            }
            else
            {
                var contentTypes = contenttypes.Split(new[] { ',' });

                IsContentCorrect = contentTypes.Any(contentType => content.ContentItem.TypeDefinition.Name == contentType);
            }

            if (IsContentCorrect)
            {
                ContentItem mainContent = (ContentItem)context.Tokens["Content"];

                if (mainContent != null && _sourcesRepository != null)
                {
                    ContentContainsList wordList = new ContentContainsList(_sourcesRepository, context);

                    return wordList.Check(mainContent.Parts);
                }
            }

            return false;
        }

        private LocalizedString Display(Models.EventContext context)
        {
            // Words
            ContentContainsList wordList = new ContentContainsList(_sourcesRepository, context);

            string words = "";
            for (int i = 0; i < wordList.List.Count(); i++)
            {
                ContentContainsItem item = wordList.List[i];
                if (item != null)
                {
                    if (i <= 0)
                    {
                        words += "\"" + item.Value + "\"";
                    }
                    else
                    {
                        words += " " + T(item.Operation.GetDisplayName()).Text;
                        words += " \"" + item.Value + "\"";
                    }
                }
            }

            // Content Type
            var contenttypes = context.Properties["contenttypes"];

            if (String.IsNullOrEmpty(contenttypes))
            {
                return T("When any content has the text ({1}).", contenttypes, words);
            }
            
            return T("When content with type(s) ({0}) has the text ({1}).", contenttypes, words);
        }
    }


}