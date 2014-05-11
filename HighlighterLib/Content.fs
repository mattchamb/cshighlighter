module Content

open System
open System.Text
open System.IO
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open HighlighterLib.Templating

type ContentUris = {
        Script: Uri;
        Style: Uri
    }

let getOrUploadLatestContent (container: CloudBlobContainer) =
    let jsPath, jsContent = 
        let content = StaticContent.GetHighlightingScript()
        let hash = Hashing.hashString content
        let path = sprintf "scripts/%s" hash
        path, Storage.Javascript (content)

    let cssPath, cssContent =
        let content = StaticContent.GetHighlightingStyle()
        let hash = Hashing.hashString content
        let path = sprintf "styles/%s" hash
        path, Storage.Css (content)
    
    let jsBlob = container.GetBlockBlobReference(jsPath)
    let cssBlob = container.GetBlockBlobReference(cssPath)
    let latestJsUri =
        if jsBlob.Exists() then
            jsBlob.Uri
        else
            Storage.storeInBlob jsBlob jsContent
    let latestCssUri = 
        if cssBlob.Exists() then
            cssBlob.Uri
        else
            Storage.storeInBlob cssBlob cssContent
    {
        Script = latestJsUri;
        Style = latestCssUri
    }

