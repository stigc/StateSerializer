Use IgnoreNamspace to stop recursive serialization, e.g.

        var ss = new StateSerializer();
        ss.IgnoreNamspace("System.Xml");
        var str = ss.ToXml(new MyClass());
