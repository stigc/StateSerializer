using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public void SerializerTest1()
        {
            var ss = new StateSerializer();
            ss.IgnoreNamspace("System.Xml");

            var str = ss.ToXml(new MyClass());
            Console.WriteLine(str);
        }
    }


    class MyBasClass : XmlDocument
    {
        protected string[] arrStrings = new string[]{ "a", "b", "c" };
    }

    class MyClass : MyBasClass
    {
        static DateTime date = DateTime.Now;
        private Type myTypeField = typeof(string);

        public XmlDocument[] arrayXml;
        public int Id { get; set; } = 1;
        public MyClass SelfReference { get; set; }
        public object NullReference { get; set; }
        public MyClass()
        {
            var xml = new XmlDocument();

            this.SelfReference = this;
            this.arrayXml = new XmlDocument[] { xml , xml, null };
        }
    }
}
