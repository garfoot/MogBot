using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace MogBot.Host.Extensions
{
    public static class ObjectExtensions
    {
        public static dynamic ToDynamic(this object source, bool useJsonAttributes = false)
        {
            IDictionary<string, object> expandoObject = new ExpandoObject();
            dynamic expando = expandoObject;

            source.GetType().GetProperties()
                  .ForEach(i=>
                  {
                      if (i == null)
                      {
                          throw new ArgumentNullException(nameof(i));
                      }

                      var name = i.Name;
                      if (useJsonAttributes)
                      {
                          var jsonProperty = i.GetCustomAttribute<JsonPropertyAttribute>();
                          if (!string.IsNullOrEmpty(jsonProperty?.PropertyName))
                          {
                              name = jsonProperty.PropertyName;
                          }
                      }

                      expandoObject.Add(name, i.GetValue(source));
                  });

            return expando;
        }

        public static Dictionary<string, object> ToDictionary(this object source)
        {
            return ((ExpandoObject) source.ToDynamic(true)).ToDictionary(i => i.Key, i => i.Value);
        }
        public static Dictionary<string, string> ToStringDictionary(this object source)
        {
            return ((ExpandoObject)source.ToDynamic(true)).ToDictionary(i => i.Key, i => i.Value?.ToString());
        }
    }
}