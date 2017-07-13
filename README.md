Use IgnoreNamspace to stop recursive serialization, e.g.

        var ss = new StateSerializer();
        ss.IgnoreNamspace("System.Xml");
        
Example

    var ss = new StateSerializer();
    ss.IgnoreNamspace("System.Xml");

    var str = ss.ToXml(new MyClass());
    Console.WriteLine(str);


        class MyBasClass : XmlDocument
        {
            protected string[] arrStrings = new string[] { "a", "b", "c" };
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
                this.arrayXml = new XmlDocument[] { xml, xml, null };
            }
        }

output

        <?xml version="1.0" encoding="utf-16"?>
        <Root Type="MyClass" Id="1">
          <IdkBackingField Type="Int32">1</IdkBackingField>
          <NullReferencekBackingField Type="Object" />
          <SelfReferencekBackingField Type="MyClass" Id="1" HasBeenSerializedBefore="true" />
          <arrayXml Type="XmlDocument[]" Id="2">
            <item Type="XmlDocument" Id="3">System.Xml.XmlDocument</item>
            <item Type="XmlDocument" Id="3">System.Xml.XmlDocument</item>
            <item />
          </arrayXml>
          <arrStrings Type="String[]" Id="4">
            <item Type="String">a</item>
            <item Type="String">b</item>
            <item Type="String">c</item>
          </arrStrings>
          <date Type="DateTime">13-07-2017 10:39:44</date>
          <myTypeField Type="Type" Id="5">System.String</myTypeField>
        </Root>

