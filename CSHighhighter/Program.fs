open System
open CSHighlighter

[<EntryPoint>]
let main argv = 
    
    let code = @"using System;

namespace TopLevel
{ 
    class Foo 
    {
        // Something something comment
        private int someField = 0;
        private string someStringField = ""asdf"";
        public Foo(int x)
        {
            var a = 10;
            a = 3456;
            this.someField = x;
        }

        /* multiline on a single line */
        /*****
        *
        * Multiline on multi lines.
        *
        ******/
        public bool Test()
        {
            #region SomeRegion
            return someField == 0 && Test();
            #endregion
        }
        private class SubClass<TSomething>
        {
            public TSomething Value { get; set; }
            public SubClass(TSomething ts)
            {
                Value = ts;
            }
        }
    }
}"

    let c2 = @"
class C<T> {
    
    protected Func<T, int> funcField = t => 999;    

    public C(string s) { 
        var a = ""lol"";
        var something = a == s;
    }

    public static C<T> Create() {
        return new C<int>(""test"");
    }

    public int Something(T t) {
        var a = this.funcField(t) * 2;
        return a;
    }
    
}"

    let c3 = @"
    class C {
    public int Length { get; set; }
    public static void Main(string[] args) {
        var asdf = args[0];
        var inst = new C();
        inst.Length = asdf.Length;
    }

}"
    let sourceFile: Analysis.SourceInput = { Path = ""; Contents = c3 }
    let a = Analysis.analyseFile(sourceFile)
    let output = Formatting.htmlFormat a

    let a = HighlighterLib.Templating.Render.SinglePage(output)
    //Console.WriteLine output
    //Console.ReadKey() |> ignore
    0 // return an integer exit code
