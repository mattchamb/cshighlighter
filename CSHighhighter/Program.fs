open System
  
[<EntryPoint>]
let main argv = 
    
    let code = @"
        using System;

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
            } 
        }"

    let a = Analysis.analyseCode(code)
    //let output = Formatting.plainFormat a
    //Console.WriteLine output
    //Console.ReadKey() |> ignore
    0 // return an integer exit code
