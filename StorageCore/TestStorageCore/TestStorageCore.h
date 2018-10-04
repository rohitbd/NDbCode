#pragma once

struct StreamInfo
{
	WCHAR StreamName[2048] = TEXT("");
};

HMODULE hStorageCore = NULL;

typedef BOOL (OpenOrCreateStorageDef)(WCHAR *storageName, IStorage** storage);
OpenOrCreateStorageDef *OpenOrCreateStorage;

typedef DWORD (GetNumStreamsDef)(IStorage *storage, StreamInfo **streamNames);
GetNumStreamsDef *GetNumStreams;

typedef BOOL (OpenOrCreateStreamDef)(IStorage *storage, WCHAR *streamName, IStream** stream);
OpenOrCreateStreamDef *OpenOrCreateStream;

typedef DWORD (UpdateStreamDef)(IStorage *storage, IStream *stream, const char *bytes, DWORD numBytes);
UpdateStreamDef *UpdateStream;

typedef void (ReadStreamDef)(IStream *stream, char **bytes, ULONG *numRead);
ReadStreamDef *ReadStream;

typedef void (CloseStorageDef)(IStorage *storage);
CloseStorageDef *CloseStorage;

typedef void (DeleteStreamDef)(IStorage *storage, const WCHAR *streamName);
DeleteStreamDef *DeleteStream;
