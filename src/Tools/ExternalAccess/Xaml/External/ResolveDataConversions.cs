﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.Json;
using Microsoft.CodeAnalysis.LanguageServer;
using Microsoft.CodeAnalysis.LanguageServer.Handler;
using Roslyn.LanguageServer.Protocol;
using Roslyn.Utilities;
using LSP = Roslyn.LanguageServer.Protocol;

namespace Microsoft.CodeAnalysis.ExternalAccess.Xaml;

internal static class ResolveDataConversions
{
    private record DataResolveData(object Data, LSP.TextDocumentIdentifier Document) : DocumentResolveData(Document);
    private record DataIdResolveData(long DataId, LSP.TextDocumentIdentifier Document) : DocumentResolveData(Document);

    public static object ToResolveData(object data, DocumentUri uri)
        => new DataResolveData(data, new LSP.TextDocumentIdentifier { DocumentUri = uri });

    public static (object? data, DocumentUri? uri) FromResolveData(object? requestData)
    {
        Contract.ThrowIfNull(requestData);
        var resolveData = JsonSerializer.Deserialize<DataResolveData>((JsonElement)requestData);
        return (resolveData?.Data, resolveData?.Document.DocumentUri);
    }

    internal static object ToCachedResolveData(object data, DocumentUri uri, ResolveDataCache resolveDataCache)
    {
        var dataId = resolveDataCache.UpdateCache(data);

        return new DataIdResolveData(dataId, new LSP.TextDocumentIdentifier { DocumentUri = uri });
    }

    internal static (object? data, DocumentUri? uri) FromCachedResolveData(object? lspData, ResolveDataCache resolveDataCache)
    {
        DataIdResolveData? resolveData;
        if (lspData is JsonElement token)
        {
            resolveData = JsonSerializer.Deserialize<DataIdResolveData>(token);
            Assumes.Present(resolveData);
        }
        else
        {
            return (null, null);
        }

        var data = resolveDataCache.GetCachedEntry(resolveData.DataId);
        var document = resolveData.Document;

        return (data, document.DocumentUri);
    }
}
