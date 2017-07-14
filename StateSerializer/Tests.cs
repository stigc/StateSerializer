using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace StateSerializerNs.Tests
{
    class Tests
    {
        [Test]
        public void AsIEnumerableShouldWork()
        {
            Assert.IsNull("abc".AsIEnumerable());
            Assert.IsNull(1.AsIEnumerable());
            Assert.IsNull(new XmlDocument().AsIEnumerable());

            Assert.IsNotNull(new int[0].AsIEnumerable());
            Assert.IsNotNull(new HashSet<string>().AsIEnumerable());
            Assert.IsNotNull(new Dictionary<int, int>().AsIEnumerable());
            Assert.IsNotNull(new ArrayList().AsIEnumerable());
            Assert.IsNotNull(new Stack().AsIEnumerable());
        }

        [Test]
        public void ShowXmlTest()
        {
            var ss = new StateSerializer();
            ss.IgnoreNamspace("System.Xml");

            var str = ss.ToXml(new MySubClass());
            Console.WriteLine(str);
        }

        [Test]
        public void IllegalXmlCharsWillBeRemoved()
        {
            var instance = new IllegalXmlChars();

            var str = new StateSerializer()
                .ToXml(instance);

            StringAssert.Contains("<ValuekBackingField Type=\"String\">AB</ValuekBackingField>", str);
            
        }

        [Test]
        public void StructShouldSerialize()
        {
            var instance = new MyStruct();

            var str = new StateSerializer()
                .ToXml(instance);

            StringAssert.Contains("<Root Type=\"MyStruct\" Id=\"1\">", str);
            StringAssert.Contains("<Id Type=\"Int32\">0</Id>", str);
        }

        [Test]
        public void IntShouldSerialize()
        {
            var str = new StateSerializer()
                .ToXml(1);

            StringAssert.Contains("<Root Type=\"Int32\">1</Root>", str);
        }
    }

    struct MyStruct  
    {
        public int Id;
    }

    class IllegalXmlChars
    {
        public string Value {get; set;} = "A\uffffB";
    }

    class MyBaseClass : XmlDocument
    {
        protected string[] arrStrings = new string[]{ "a", "b", "c" };
    }

    class MySubClass : MyBaseClass
    {
        static DateTime date = DateTime.Now;
        private Type myTypeField = typeof(string);
        public object[] arrayXml;
        public int Id { get; set; } = 1;
        public MySubClass SelfReference { get; set; }
        public object NullReference { get; set; }
        public MySubClass()
        {
            var xml = new XmlDocument();

            this.SelfReference = this;
            this.arrayXml = new object[] { xml , xml, "abc", null };
        }
    }
}
