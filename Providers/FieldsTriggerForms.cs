using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Orchard.Core.Settings.Metadata.Records;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Rules.Providers
{
    public static class Extensions
    {
        public static string GetDisplayName(this Enum data)
        {

            MemberInfo memberInfo = data.GetType().GetMember(data.ToString()).FirstOrDefault();

            if (memberInfo != null)
            {
                DisplayAttribute attribute = (DisplayAttribute) memberInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
                return attribute.Name;
            }

            return string.Empty;
        }
    }

    public class FieldsTriggerForms : IFormProvider {
        private readonly IRepository<ContentPartDefinitionRecord> _sourcesRepository;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public FieldsTriggerForms(IShapeFactory shapeFactory, IRepository<ContentPartDefinitionRecord> sources)
        {
            _sourcesRepository = sources;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public enum Triggers
        {
            [Display(Name = "Fonte contem o texto")]
            SourceContainsText
        }

        public enum Actions
        {
            [Display(Name = "Envia Email")]
            SendEmail,

            [Display(Name = "Envia Tweet")]
            SendTweet
        }

        public void Describe(DescribeContext context)
        {
            context.Form("ActionFieldsTrigger",
                shape =>
                {
                    var form = Shape.Form(
                        Id: "ActionFieldsTrigger",
                        _Name: Shape.Textbox(
                            Id: "Name", Name: "Name",
                            Title: T("Nome"),
                            Classes: new[] { "textMedium" }),
                        _Source: Shape.SelectList(
                            Id: "Source", Name: "Source",
                            Title: T("Fonte")),
                        _Trigger: Shape.SelectList(
                            Id: "Trigger", Name: "Trigger",
                            Title: T("Gatilho")),
                        _TriggerData: Shape.Textbox(
                            Id: "TriggerData", Name: "TriggerData",
                            Classes: new[] { "textMedium" }),
                        _Action: Shape.SelectList(
                            Id: "Action", Name: "Action",
                            Title: T("Ação")),
                        _ActionData: Shape.Textbox(
                            Id: "ActionData", Name: "ActionData",
                            Classes: new[] { "textMedium" })
                        );

                    // Sources
                    List<ContentPartDefinitionRecord> sources = _sourcesRepository.Table.Where(x => x.Hidden == false).OrderBy(x => x.Name).ToList();

                    foreach (ContentPartDefinitionRecord source in sources)
                    {
                        form._Source.Add(new SelectListItem { Value = source.Id.ToString(), Text = source.Name });
                    }
                    
                    // Triggers
                    foreach (Triggers trigger in Enum.GetValues(typeof(Triggers)))
                    {
                        form._Trigger.Add(new SelectListItem { Value = ((int)trigger).ToString(), Text = trigger.GetDisplayName() });
                    }

                    // Actions
                    foreach (Actions action in Enum.GetValues(typeof(Actions)))
                    {
                        form._Action.Add(new SelectListItem { Value = ((int)action).ToString(), Text = action.GetDisplayName() });
                    }

                    return form;
                }
            );

            context.Form("ActionFieldsTrigger2",
                shape => Shape.Form(
                Id: "ActionFieldsTrigger2",
                _Date: Shape.Textbox(
                    Id: "Date", Name: "Date",
                    Title: T("Date")),
                _Time: Shape.Textbox(
                    Id: "Time", Name: "Time",
                    Title: T("Time"))
                )
            );
        }
    }

    public class FieldsTriggerValitator : FormHandler
    {
        public Localizer T { get; set; }

        public override void Validating(ValidatingContext context)
        {
            
            if (context.FormName == "ActionFieldsTrigger")
            {
                /*
                if (context.ValueProvider.GetValue("Amount").AttemptedValue == String.Empty)
                {
                    context.ModelState.AddModelError("Amount", T("You must provide an Amount").Text);
                }

                if (context.ValueProvider.GetValue("Unity").AttemptedValue == String.Empty)
                {
                    context.ModelState.AddModelError("Unity", T("You must provide a Type").Text);
                }

                if (context.ValueProvider.GetValue("RuleId").AttemptedValue == String.Empty)
                {
                    context.ModelState.AddModelError("RuleId", T("You must select at least one Rule").Text);
                }
                 * */
            }
            
        }
    }
}