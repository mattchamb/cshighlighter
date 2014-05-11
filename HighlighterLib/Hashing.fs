namespace CSHighlighter
module Hashing =

    open System
    open System.Text
    open System.Security.Cryptography

    let hashData (input: byte array) =
        use sha = SHA1.Create()
        let hash = sha.ComputeHash(input)
        let x = new StringBuilder()
        for bte in hash do
            x.Append(bte.ToString("x2")) |> ignore
        x.ToString()

    let hashString (input: string) =
        let data = Encoding.UTF8.GetBytes input
        hashData data
