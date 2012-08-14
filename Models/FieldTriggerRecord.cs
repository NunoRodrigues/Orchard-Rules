using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data;

namespace Orchard.Rules.Models
{
    public class FieldTriggerRecord
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int ContentPartDefinitionRecord_Id { get; set; }
        public virtual int TriggerType { get; set; }
        public virtual string TriggerData { get; set; }
        public virtual int ActionType { get; set; }
        public virtual string ActionData { get; set; }
    }
}