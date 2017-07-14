using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace StateSerializerNs
{
    public class StateSerializer
    {
        private int nextId;
        Dictionary<object, int> dic;
        XmlWriter xw;

        public int MaxListElementsToSerialize { get; set; }
        List<string> NamespacesToIgnore = new List<string>();

        public String ToXml(Object root)
        {
            nextId = 0;
            dic = new Dictionary<object, int>(new IdentityEqualityComparer());

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };

            var sb = new StringBuilder();

            using (xw = XmlWriter.Create(sb, settings))
            {
                PrintClassRecursive(root, 0, "Root");
            }

            return sb.ToString();
        }

        public void IgnoreNamspace(String ns)
        {
            NamespacesToIgnore.Add(ns);
        }

        private string getIdTag(object value)
        {
            if (value == null)
                return null;

            if (dic.ContainsKey(value) == false)
            {
                nextId++;
                dic.Add(value, nextId);
            }

            return dic[value].ToString();
        }

        private void PrintClassRecursive(object current, int level, String nodeName)
        {
            xw.WriteStartElement(nodeName);

            if (current != null)
                xw.WriteAttributeString("Type", current.GetType().Name);

            var hasBeenSerializedBefore = WriteIdTagAsAttribute(current);

            if (current.ShouldNotBeTraversed(NamespacesToIgnore))
            {
                xw.WriteString(current.ToXmlValueStr());
                xw.WriteEndElement();
                return;
            }

            if (hasBeenSerializedBefore)
            {
                xw.WriteAttributeString("HasBeenSerializedBefore", "true");
                xw.WriteEndElement();
                return;
            }

            foreach (var field in GetFieldsRecursive(current.GetType()))
            {
                var value = field.GetValue(current);
                var list = value.AsIEnumerable();

                if (list != null)
                {
                    xw.WriteStartElement(field.NameToStr());
                    xw.WriteAttributeString("Type", field.FieldType.Name);
                    WriteIdTagAsAttribute(value);
                    foreach (var arrayValue in value.AsIEnumerable())
                    {
                        PrintClassRecursive(arrayValue, level + 1, "item");
                    }
                    xw.WriteEndElement();
                }
                else
                {
                    if (value.ShouldNotBeTraversed(NamespacesToIgnore))
                    {
                        xw.WriteStartElement(field.NameToStr());
                        xw.WriteAttributeString("Type", field.FieldType.Name);
                        WriteIdTagAsAttribute(value);
                        xw.WriteString(value.ToXmlValueStr());
                        xw.WriteEndElement();
                    }
                    else
                        PrintClassRecursive(value, level + 1, field.NameToStr());
                }
            }

            xw.WriteEndElement();
        }

        private bool WriteIdTagAsAttribute(object value)
        {
            //strings and value-types should not have ID tag
            if (value == null || value.IsSystemValueType() || value is string)
                return false;

            bool isKnown = dic.ContainsKey(value);

            var id = getIdTag(value);

            if (id != null)
                xw.WriteAttributeString("Id", id);

            return isKnown;
        }

        private List<FieldInfo> GetFieldsRecursive(Type type)
        {
            if (NamespacesToIgnore.Any(ns => type.Namespace.StartsWith(ns)))
                return new List<FieldInfo>();

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static)
                .ToList();

            if (type.BaseType == null)
                return fields;

            fields.AddRange(GetFieldsRecursive(type.BaseType));

            return fields.OrderBy(x => x.Name)
                .ThenBy(x => x.MemberType)
                .ToList();
        }
    }

    class IdentityEqualityComparer : IEqualityComparer<object>
    {
        public bool Equals(object o1, object o2)
        {
            if (o1 != null && o1.GetType().IsValueType)
                return o1.Equals(o2);

            return Object.ReferenceEquals(o1, o2);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }

    static class ExtensionMehtods
    {
        public static bool ShouldNotBeTraversed(this object value, List<string> namespacesToIgnore)
        {
            return value == null
                        || value is string
                        || IsSystemValueType(value)
                        || value is Type
                        || value is System.Reflection.Pointer
                        || value.GetType().BaseType == typeof(MulticastDelegate)
                        || namespacesToIgnore.Any(x => value.GetType().Namespace.StartsWith(x));
        }

        public static bool IsSystemValueType(this object value)
        {
            return value is bool
                || value is byte
                || value is char
                || value is decimal
                || value is double
                || value is Enum
                || value is float
                || value is int
                || value is long
                || value is sbyte
                || value is short
                || value is uint
                || value is ulong
                || value is ushort
                || value is DateTime
                || value is TimeSpan;



        }

        public static string ToXmlValueStr(this object value)
        {
            if (value == null)
                return null;

            return value.ToString().RemoveInvalidXMLChars();
        }

        public static IEnumerable AsIEnumerable(this object value)
        {
            if (value is string)
                return null;
            if (value is XmlDocument)
                return null;
            return value as IEnumerable;
        }

        public static string NameToStr(this FieldInfo value)
        {
            var name = value.Name;

            if (string.IsNullOrWhiteSpace(name))
                return "noname";

            name = new String(value.Name.Where(Char.IsLetter).ToArray());

            if (string.IsNullOrWhiteSpace(name))
                return "noname";

            return name;
        }

        private static Regex _invalidXMLChars = new Regex(
                @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
                RegexOptions.Compiled);

        public static string RemoveInvalidXMLChars(this string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return _invalidXMLChars.Replace(text, "");
        }
    }
}
