using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web.Mvc;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Rules.Models;
using Orchard.Data;
using Orchard.Core.Settings.Metadata.Records;

namespace Orchard.Rules.Events
{
    public static class Extensions
    {
        public static string GetDisplayName(this Enum data)
        {
            MemberInfo memberInfo = data.GetType().GetMember(data.ToString()).FirstOrDefault();

            if (memberInfo != null)
            {
                DisplayAttribute attribute = (DisplayAttribute)memberInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
                return attribute.Name;
            }

            return string.Empty;
        }
    }

    public class ContentContainsForms : IFormProvider
    {
        private readonly IRepository<ContentPartDefinitionRecord> _sourcesRepository;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ContentContainsForms(
            IShapeFactory shapeFactory,
            IContentDefinitionManager contentDefinitionManager,
            IRepository<ContentPartDefinitionRecord> sourcesRepository)
        {
            Shape = shapeFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _sourcesRepository = sourcesRepository;
            T = NullLocalizer.Instance;
        }

        /// <summary>
        /// Generates the form shown on the rules admin area
        /// </summary>
        /// <param name="context"></param>
        public void Describe(DescribeContext context)
        {
            Func<IShapeFactory, dynamic> form =
                shape =>
                {

                    var f = Shape.Form(
                        Id: "AnyOfContentTypes",
                        _Parts: Shape.SelectList(
                            Id: "contenttypes", Name: "contenttypes",
                            Title: T("Content types"),
                            Description: T("Select some content types."),
                            Size: 10,
                            Multiple: true
                            ),
                        _Word1: Shape.FieldSet(
                            Id: ContentContainsItem.GetGroupName(1),
                            Title: T("Search for words"),
                            _Source: Shape.SelectList(
                                Id: ContentContainsItem.GetSourceName(1), Name: ContentContainsItem.GetSourceName(1),
                                Multiple: false
                                ),
                            _Value: Shape.TextBox(
                                Id: ContentContainsItem.GetValueName(1), Name: ContentContainsItem.GetValueName(1),
                                Classes: new[] { "textMedium", "tokenized" }
                                )
                            ),
                        _Word2: Shape.FieldSet(
                            Id: ContentContainsItem.GetGroupName(2),
                            _Operation: Shape.SelectList(
                                Id: ContentContainsItem.GetOperationName(2), Name: ContentContainsItem.GetOperationName(2),
                                Size: 1,
                                Multiple: false
                                ),
                            _Source: Shape.SelectList(
                                Id: ContentContainsItem.GetSourceName(2), Name: ContentContainsItem.GetSourceName(2),
                                Multiple: false
                                ),
                            _Value: Shape.TextBox(
                                Id: ContentContainsItem.GetValueName(2), Name: ContentContainsItem.GetValueName(2),
                                Classes: new[] { "textMedium", "tokenized" }
                                )
                            ),
                        _Word3: Shape.FieldSet(
                            Id: ContentContainsItem.GetGroupName(3),
                            _Operation: Shape.SelectList(
                                Id: ContentContainsItem.GetOperationName(3), Name: ContentContainsItem.GetOperationName(3),
                                Size: 1,
                                Multiple: false
                                ),
                            _Source: Shape.SelectList(
                                Id: ContentContainsItem.GetSourceName(3), Name: ContentContainsItem.GetSourceName(3),
                                Multiple: false
                                ),
                            _Value: Shape.TextBox(
                                Id: ContentContainsItem.GetValueName(3), Name: ContentContainsItem.GetValueName(3),
                                Classes: new[] { "textMedium", "tokenized" }
                                )
                            )
                        );

                    f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions())
                    {
                        f._Parts.Add(new SelectListItem { Value = contentType.Name, Text = contentType.DisplayName });
                    }

                    // Sources
                    List<ContentPartDefinitionRecord> sources = _sourcesRepository.Table.Where(x => x.Hidden == false && x.Settings != null).OrderBy(x => x.Name).ToList();

                    foreach (ContentPartDefinitionRecord source in sources)
                    {
                        f._Word1._Source.Add(new SelectListItem { Value = source.Id.ToString(), Text = source.Name.Replace("Part","") });
                        f._Word2._Source.Add(new SelectListItem { Value = source.Id.ToString(), Text = source.Name.Replace("Part", "") });
                        f._Word3._Source.Add(new SelectListItem { Value = source.Id.ToString(), Text = source.Name.Replace("Part", "") });
                    }

                    // Operators
                    foreach (ContentContainsItem.SearchOperators op in Enum.GetValues(typeof(ContentContainsItem.SearchOperators)))
                    {
                        f._Word2._Operation.Add(new SelectListItem { Value = ((int)op).ToString(), Text = op.GetDisplayName() });
                        f._Word3._Operation.Add(new SelectListItem { Value = ((int)op).ToString(), Text = op.GetDisplayName() });
                    }

                    return f;
                };
            //_XPTO : Shape.Markup(
            //                Value : "<span><b>YEAH!</b></span>"
            //            ),

            

            context.Form("SelectContentTypes", form);
        }
    }
}