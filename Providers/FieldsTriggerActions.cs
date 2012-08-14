using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Rules.Models;
using Orchard.Rules.Services;

using Orchard.Data;
using Orchard.Services;
using Orchard.Mvc.Html;
using Orchard.Tasks.Scheduling;
using Orchard.Tokens;
using Orchard.Core.Settings.Metadata.Records;

namespace Orchard.Rules.Rules
{

    public class FieldsTriggerActions : IActionProvider
    {
        private readonly IContentManager _contentManager;

        public FieldsTriggerActions(IContentManager contentManager)
        {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeActionContext context)
        {
            context.For("System", T("System"), T("System"))
                .Element("FieldsTrigger", T("Ações sensiveis ao contexto"), T("Ações sensiveis ao contexto"), Action, Display, "ActionFieldsTrigger");
        }

        private bool Action(ActionContext context)
        {
            int Id = Convert.ToInt32(0);
            string name = context.Properties["Name"];
            int triggerType = Convert.ToInt32(context.Properties["Trigger"]);
            string triggerData = context.Properties["TriggerData"];
            int actionType = Convert.ToInt32(context.Properties["Action"]);
            string actionData = context.Properties["ActionData"];

            return true;
        }

        private LocalizedString Display(ActionContext context) 
        {
            return T.Plural("Action Power! {1}", "Action Power! {1}", 1);
        }
    }
}