using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoDatabase;
using System.Runtime.Serialization;

namespace TestNoDatabase
{
    [Serializable]
    class Test : NoDatabaseEntity
    {
        public override object EntityKey
        {
            get; set;
        }

        public string Name { get; set; }
    }

    class Program
    {
        static void OnContextCreate(string contextName, out DataStore ds)
        {
            ds = new DataStore("H:\\TestDb");
        }

        static void Main(string[] args)
        {
            Test t = new Test() { EntityKey = 0, Name = "ABCD" };

            ContextFactory.Instance.OnCreateContext = OnContextCreate;

            //DataStore ds = new DataStore("H:\\TestDb");
            ExecutionContext ctx = ContextFactory.Instance["MyTest"];

            DataStore ds = ctx.Connection;

            Test[] all = ds.GetAll<Test>();

            bool retVal;

            //ds.Insert(t, "ID");
            retVal = ds.Save(t, "item1");

            all = ds.GetAll<Test>();

            if (all.Length > 0)
                Console.WriteLine(all[0].Name);

            t.Name = "XYZ";

            //ds.Update(t, "ID");
            retVal = ds.Save(t, "item1");

            all = ds.GetAll<Test>();

            if (all.Length > 0)
                Console.WriteLine(all[0].Name);

            Test item = ds.Get<Test>("item1");

            ds.Delete(item);

            all = ds.GetAll<Test>();
        }
    }
}
