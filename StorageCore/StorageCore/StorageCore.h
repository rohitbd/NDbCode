// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the STORAGECORE_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// STORAGECORE_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef STORAGECORE_EXPORTS
#define STORAGECORE_API extern "C" __declspec(dllexport)
#else
#define STORAGECORE_API __declspec(dllimport)
#endif

std::atomic<BOOL> isLocked = FALSE;

BOOL lock();
void unlock();

struct StreamInfo
{
	WCHAR StreamName[2048] = TEXT("");
};

STORAGECORE_API BOOL OpenOrCreateStorage(const WCHAR* storageName, IStorage**);
STORAGECORE_API DWORD GetNumStreams(IStorage *storage, StreamInfo **streamNames);
STORAGECORE_API BOOL OpenOrCreateStream(IStorage *storage, const WCHAR *streamName, IStream** stream);
STORAGECORE_API DWORD UpdateStream(IStorage *storage, IStream *stream, const char *bytes, DWORD numBytes);
STORAGECORE_API void ReadStream(IStream *stream, char **bytes, ULONG *numRead);
STORAGECORE_API void DeleteStream(IStorage *storage, const WCHAR *streamName);
STORAGECORE_API void CloseStream(IStream *stream);
STORAGECORE_API void CloseStorage(IStorage *storage);
