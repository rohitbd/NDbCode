// StorageCore.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "StorageCore.h"

BOOL lock()
{
	return (!isLocked.exchange(TRUE));
}

void unlock()
{
	isLocked = FALSE;
}

STORAGECORE_API BOOL OpenOrCreateStorage(const WCHAR *storageName, IStorage** storage)
{
	HRESULT result;

	STGOPTIONS stgOptions;

	stgOptions.usVersion = 1;
	stgOptions.reserved = 0;
	stgOptions.pwcsTemplateFile = NULL;
	stgOptions.ulSectorSize = 4096;

	result = StgOpenStorageEx(storageName, STGM_DIRECT | STGM_READWRITE | STGM_SHARE_EXCLUSIVE, STGFMT_STORAGE, 0, /*&stgOptions*/NULL, NULL, IID_IStorage, reinterpret_cast<void**>(storage));

	if (result != S_OK || storage == NULL)
	{
		result = StgCreateStorageEx(storageName, STGM_DIRECT | STGM_CREATE | STGM_SHARE_EXCLUSIVE | STGM_READWRITE, STGFMT_STORAGE, 0, /*&stgOptions*/NULL, NULL, IID_IStorage, reinterpret_cast<void**>(storage));
	}

	if (result != S_OK || storage == NULL)
		return FALSE;

	return TRUE;
}

STORAGECORE_API DWORD GetNumStreams(IStorage *storage, StreamInfo** streamNames)
{
	lock();

	IEnumSTATSTG *ienumStgStat = NULL;

	HRESULT hr = storage->EnumElements(0, NULL, 0, &ienumStgStat);

	if (hr != S_OK)
		return 0;

	STATSTG statStg[1];

	ULONG pFetched = 0;

	DWORD retVal = 0;

	while ((hr = ienumStgStat->Next(1, statStg, &pFetched)) == S_OK)
	{
		if (pFetched == 0)
			break;

		retVal++;
	}

	(*streamNames) = new StreamInfo[retVal];

	hr = storage->EnumElements(0, NULL, 0, &ienumStgStat);

	pFetched = 0;

	int i = 0;

	while ((hr = ienumStgStat->Next(1, statStg, &pFetched)) == S_OK)
	{
		if (pFetched == 0)
			break;

		lstrcpynW((*streamNames)[i].StreamName, statStg->pwcsName, 2048);

		i++;
	}

	unlock();

	return retVal;
}

STORAGECORE_API BOOL OpenOrCreateStream(IStorage *storage, const WCHAR *streamName, IStream** stream)
{
	lock();

	HRESULT hr = storage->OpenStream(streamName, 0, STGM_DIRECT | STGM_SHARE_EXCLUSIVE | STGM_READWRITE, 0, stream);

	if (hr != S_OK || stream == NULL)
	{
		hr = storage->CreateStream(streamName, STGM_DIRECT | STGM_CREATE | STGM_SHARE_EXCLUSIVE | STGM_READWRITE, 0, 0, stream);

		if (hr != S_OK || stream == NULL)
		{
			unlock();
			return FALSE;
		}

		hr = (*stream)->Commit(STGC_OVERWRITE);

		if (hr == S_OK)
		{
			hr = storage->Commit(STGC_DEFAULT);
		}
	}

	unlock();

	if (hr != S_OK || stream == NULL)
		return NULL;

	return TRUE;
}

STORAGECORE_API DWORD UpdateStream(IStorage *storage, IStream *stream, const char *bytes, DWORD numBytes)
{
	lock();

	DWORD numWritten = 0;
	/*LARGE_INTEGER li;

	li.HighPart = 0;
	li.LowPart = 0;
	li.QuadPart = 0;
	li.u.HighPart = 0;
	li.u.LowPart = 0;

	stream->Seek(li, 0, NULL);*/

	if (stream->Write(bytes, numBytes, &numWritten) == S_OK)
	{
		//stream->Seek(li, 0, NULL);

		if (stream->Commit(STGC_DEFAULT) == S_OK)
		{
			if (storage->Commit(STGC_DEFAULT) != S_OK)
			{
				//What to do?
			}

			stream->Release();
		}
	}

	unlock();

	return numWritten;
}

STORAGECORE_API void ReadStream(IStream *stream, char **bytes, ULONG *numRead)
{
	lock();

	(*bytes) = NULL;

	*numRead = 0;
	
	STATSTG stat;

	HRESULT hr = stream->Stat(&stat, STATFLAG_DEFAULT);

	(*bytes) = new char[stat.cbSize.LowPart];

	memset((*bytes), 0, stat.cbSize.LowPart);

	/*LARGE_INTEGER li;

	li.HighPart = 0;
	li.LowPart = 0;
	li.QuadPart = 0;
	li.u.HighPart = 0;
	li.u.LowPart = 0;

	stream->Seek(li, 0, NULL);*/

	stream->Read((*bytes), stat.cbSize.LowPart, numRead);

	stream->Release();

	unlock();
}

STORAGECORE_API void CloseStream(IStream *stream)
{
	stream->Commit(STGC_DEFAULT);
	stream->Release();
}

STORAGECORE_API void DeleteStream(IStorage *storage, const WCHAR *streamName)
{
	lock();

	storage->DestroyElement(streamName);

	storage->Commit(STGC_DEFAULT);

	unlock();
}

STORAGECORE_API void CloseStorage(IStorage *storage)
{
	storage->Commit(STGC_DEFAULT);

	storage->Release();
}