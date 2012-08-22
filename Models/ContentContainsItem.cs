using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Orchard.Rules.Models
{

    public class ContentContainsItem
    {
        public enum SearchOperators
        {
            [Display(Name = "And")]
            And,

            [Display(Name = "Or")]
            Or
        }

        private SearchOperators _operation;
        public SearchOperators Operation { get { return _operation; } }

        private int _contentPartId;
        public int ContentPartId { get { return _contentPartId; } }

        private string _value;
        public string Value { get { return _value; } }

        private List<string> _tests;
        public List<string> Tests { get { return _tests; } }

        public ContentContainsItem(SearchOperators operation, int contentPartId, string value)
        {
            _operation = operation;
            _contentPartId = contentPartId;
            _value = value;
            _tests = new List<string>();
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

        /// <summary>
        /// Loads the value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ContentContainsItem Load(Models.EventContext context, int id)
        {
            string valueName = GetValueName(id);
            if (context.Properties.ContainsKey(valueName))
            {
                string value = context.Properties[valueName];

                if (string.IsNullOrEmpty(value) == false) // If no value is defined then this condition is not important
                {
                    SearchOperators operation = SearchOperators.And; // Default Condition

                    string operationName = GetOperationName(id);
                    if (context.Properties.ContainsKey(operationName))
                    {
                        operation = (SearchOperators)int.Parse(context.Properties[operationName]);
                    }

                    int sourceID = 0;
                    string sourceName = GetSourceName(id);
                    if (context.Properties.ContainsKey(sourceName))
                    {
                        sourceID = int.Parse(context.Properties[sourceName]);
                    }

                    return new ContentContainsItem(operation, sourceID, value);
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