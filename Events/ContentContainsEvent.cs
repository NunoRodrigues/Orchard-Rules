using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
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
        public ContentContainsEvent()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(Models.DescribeEventContext describe)
        {
            Func<Models.EventContext, bool> condition = Condition;
            Func<Models.EventContext, LocalizedString> display = (Func<Models.EventContext, LocalizedString>)(context => Display(context));

            describe.For("Content", T("Content Items"), T("Content Items")).Element("Contains", T("Content Contains"), T("Content contains text"), condition, display, "SelectContentTypes");
        }

        private static bool Condition(Models.EventContext context)
        {
            bool IsContentCorrect = false;
            string contenttypes = context.Properties["contenttypes"];
            var content = context.Tokens["Content"] as IContent;

            // "" means 'any'
            if (String.IsNullOrEmpty(contenttypes))
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
                // Pesquisa nas Parts que podem ter conteudo pelas palavras
                var words = context.Properties["ContainsWords"];

                ContentItem mainContent = (ContentItem)context.Tokens["Content"];

                if (mainContent != null)
                {
                    foreach(ContentPart part in mainContent.Parts) {
                        Type type = part.GetType();
                        
                        // Title
                        if (type == typeof(Core.Title.Models.TitlePart))
                        {
                            Core.Title.Models.TitlePart title = (Core.Title.Models.TitlePart)part;

                            if(title.Title.Contains(words)) {
                                return true;
                            }
                        }

                        // Body
                        if (type == typeof(Core.Common.Models.BodyPart))
                        {
                            Core.Common.Models.BodyPart body = (Core.Common.Models.BodyPart)part;

                            if (body.Text.Contains(words))
                            {
                                return true;
                            }
                        }

                        /*
                        // Tags
                        if (type == typeof(Orchard.Tags.Models.TagsPart))
                        {
                        
                        }
                        */
                    }
                }
            }

            return false;
        }

        private LocalizedString Display(Models.EventContext context)
        {
            // Content Type
            var contenttypes = context.Properties["contenttypes"];

            // Words
            var words = context.Properties["ContainsWords"];


            if (String.IsNullOrEmpty(contenttypes))
            {
                return T("When any content has the text ({1}).", contenttypes, words);
            }
            
            return T("When content with types ({0}) has the text ({1}).", contenttypes, words);
        }
    }


}