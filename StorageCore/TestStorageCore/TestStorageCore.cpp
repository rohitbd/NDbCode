// TestStorageCore.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "TestStorageCore.h"

int main()
{
	hStorageCore = LoadLibrary(L"StorageCore.dll");

	if (hStorageCore == NULL)
		return -1;

	OpenOrCreateStorage = (OpenOrCreateStorageDef*)GetProcAddress(hStorageCore, "OpenOrCreateStorage");
	GetNumStreams = (GetNumStreamsDef*)GetProcAddress(hStorageCore, "GetNumStreams");
	OpenOrCreateStream = (OpenOrCreateStreamDef*)GetProcAddress(hStorageCore, "OpenOrCreateStream");
	UpdateStream = (UpdateStreamDef*)GetProcAddress(hStorageCore, "UpdateStream");
	ReadStream = (ReadStreamDef*)GetProcAddress(hStorageCore, "ReadStream");
	CloseStorage = (CloseStorageDef*)GetProcAddress(hStorageCore, "CloseStorage");
	DeleteStream = (DeleteStreamDef*)GetProcAddress(hStorageCore, "DeleteStream");

	IStorage *storage = NULL;
	
	OpenOrCreateStorage(L"H:\\TestDb\\MyStorage.stg", &storage);

	StreamInfo *streamNames = NULL;

	IStream *stream = NULL;

	DeleteStream(storage, L"MyStream");
	
	OpenOrCreateStream(storage, L"MyStream", &stream);

	char* test = "hello world123\r\nhello world123\r\nhello world123\r\nhello world123\r\nhello world123hello world123\r\nhello world123\r\n";

	DWORD numWritten = UpdateStream(storage, stream, test, strlen(test));

	OpenOrCreateStream(storage, L"MyStream", &stream);

	char *bytes = NULL;

	ULONG numRead = 0;

	ReadStream(stream, &bytes, &numRead);

	OpenOrCreateStream(storage, L"MyStream1", &stream);

	printf("%s", bytes);

	DWORD numStreams = GetNumStreams(storage, &streamNames);

	CloseStorage(storage);

	getchar();

    return 0;
}

