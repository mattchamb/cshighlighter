﻿namespace Highlighter.Web

module Storage =

    open System
    open System.Text
    open System.IO
    open Microsoft.WindowsAzure.Storage
    open Microsoft.WindowsAzure.Storage.Blob

    type BlobContents =
        | Html of string
        | Css of string
        | Javascript of string

    let getContainer (client: CloudBlobClient) (containerName: string) : CloudBlobContainer =
        let container = client.GetContainerReference(containerName)
        container.CreateIfNotExists(BlobContainerPublicAccessType.Blob) |> ignore
        container

    let storeInBlob (blob: ICloudBlob) (contents: BlobContents) =
        let uploadToBlob (destBlob: ICloudBlob) (contents: string) (contentType: string) =
            use stream = new MemoryStream(Encoding.UTF8.GetBytes contents)
            destBlob.Properties.ContentType <- contentType
            destBlob.UploadFromStream(stream)
            destBlob.SetProperties()
        match contents with
        | Html c -> uploadToBlob blob c "text/html"
        | Css c -> uploadToBlob blob c "text/css"
        | Javascript c -> uploadToBlob blob c "text/javascript"
        blob.Uri

    let storeBlob (container: CloudBlobContainer) (blobPath: string) (contents: BlobContents) =
        let blob = container.GetBlockBlobReference(blobPath)
        storeInBlob blob contents

