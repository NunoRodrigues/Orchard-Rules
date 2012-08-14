using System;
using System.Web.Mvc;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Rules.Events
{
    public class ContentContainsForms : IFormProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ContentContainsForms(
            IShapeFactory shapeFactory,
            IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context)
        {
            Func<IShapeFactory, dynamic> form =
                shape =>
                {

                    var f = Shape.Form(
                        Id: "AnyOfContentTypes",
                        _Parts: Shape.SelectList(
                            Id: "contenttypes", Name: "contenttypes",
                            Title: T("Content types 2222"),
                            Description: T("Select some content types."),
                            Size: 10,
                            Multiple: true
                            ),
                        _ActionData: Shape.Textbox(
                            Id: "ContainsWords", Name: "ContainsWords",
                            Title: T("Contains Words"),
                            Classes: new[] { "textMedium" })
                        );

                    f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions())
                    {
                        f._Parts.Add(new SelectListItem { Value = contentType.Name, Text = contentType.DisplayName });
                    }

                    return f;
                };

            context.Form("SelectContentTypes", form);

        }
    }
}