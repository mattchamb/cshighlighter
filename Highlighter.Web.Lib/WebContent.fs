namespace Highlighter.Web

module WebContent =

    open System
    open System.Text
    open System.IO
    open Microsoft.WindowsAzure.Storage
    open Microsoft.WindowsAzure.Storage.Blob
    open Storage

    type ContentUris = {
            Script: Uri;
            Style: Uri
        }

    let getOrUploadContent (container: CloudBlobContainer) (projectId: string) (jsContent: string) (cssContent: string) =
        let jsBlobPath = 
            let hash = Hashing.hashString jsContent
            let path = sprintf "%s/scripts/%s" projectId hash
            path

        let cssBlobPath =
            let hash = Hashing.hashString cssContent
            let path = sprintf "%s/styles/%s" projectId hash
            path
    
        let jsBlob = container.GetBlockBlobReference(jsBlobPath)
        let cssBlob = container.GetBlockBlobReference(cssBlobPath)

        let storeOrGetAddress (blob: ICloudBlob) blobContents =
            match blob.Exists() with
            | true -> blob.Uri
            | false -> Storage.storeInBlob blob blobContents

        let jsUri = storeOrGetAddress jsBlob (Javascript jsContent)
        let cssUri = storeOrGetAddress cssBlob (Css cssContent)
        { Script = jsUri; Style = cssUri }
