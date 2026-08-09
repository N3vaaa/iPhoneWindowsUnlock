#include "iPhoneCredentialProvider.h"

#include <new>
#include <unknwn.h>

IPhoneCredentialProvider::IPhoneCredentialProvider()
    : _refCount(1)
{
}

IPhoneCredentialProvider::~IPhoneCredentialProvider()
{
}

IFACEMETHODIMP IPhoneCredentialProvider::QueryInterface(
    REFIID riid,
    void** ppv)
{
    if (ppv == nullptr)
        return E_POINTER;

    *ppv = nullptr;

    if (riid == IID_IUnknown ||
        riid == __uuidof(ICredentialProvider))
    {
        *ppv = static_cast<ICredentialProvider*>(this);
        AddRef();
        return S_OK;
    }

    return E_NOINTERFACE;
}

IFACEMETHODIMP_(ULONG)
IPhoneCredentialProvider::AddRef()
{
    return static_cast<ULONG>(
        InterlockedIncrement(&_refCount));
}

IFACEMETHODIMP_(ULONG)
IPhoneCredentialProvider::Release()
{
    ULONG count = static_cast<ULONG>(
        InterlockedDecrement(&_refCount));

    if (count == 0)
        delete this;

    return count;
}

IFACEMETHODIMP
IPhoneCredentialProvider::SetUsageScenario(
    CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
    DWORD dwFlags)
{
    UNREFERENCED_PARAMETER(cpus);
    UNREFERENCED_PARAMETER(dwFlags);

    return E_NOTIMPL;
}

IFACEMETHODIMP
IPhoneCredentialProvider::SetSerialization(
    const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs)
{
    UNREFERENCED_PARAMETER(pcpcs);

    return E_NOTIMPL;
}

IFACEMETHODIMP
IPhoneCredentialProvider::Advise(
    ICredentialProviderEvents* pcpe,
    UINT_PTR upAdviseContext)
{
    UNREFERENCED_PARAMETER(pcpe);
    UNREFERENCED_PARAMETER(upAdviseContext);

    return S_OK;
}

IFACEMETHODIMP
IPhoneCredentialProvider::UnAdvise()
{
    return S_OK;
}

IFACEMETHODIMP
IPhoneCredentialProvider::GetFieldDescriptorCount(
    DWORD* pdwCount)
{
    if (pdwCount == nullptr)
        return E_POINTER;

    *pdwCount = 0;

    return S_OK;
}

IFACEMETHODIMP
IPhoneCredentialProvider::GetFieldDescriptorAt(
    DWORD dwIndex,
    CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd)
{
    UNREFERENCED_PARAMETER(dwIndex);

    if (ppcpfd == nullptr)
        return E_POINTER;

    *ppcpfd = nullptr;

    return E_NOTIMPL;
}

IFACEMETHODIMP
IPhoneCredentialProvider::GetCredentialCount(
    DWORD* pdwCount,
    DWORD* pdwDefault,
    BOOL* pbAutoLogonWithDefault)
{
    if (pdwCount == nullptr ||
        pdwDefault == nullptr ||
        pbAutoLogonWithDefault == nullptr)
    {
        return E_POINTER;
    }

    *pdwCount = 0;
    *pdwDefault = 0;
    *pbAutoLogonWithDefault = FALSE;

    return S_OK;
}

IFACEMETHODIMP
IPhoneCredentialProvider::GetCredentialAt(
    DWORD dwIndex,
    ICredentialProviderCredential** ppcpc)
{
    UNREFERENCED_PARAMETER(dwIndex);

    if (ppcpc == nullptr)
        return E_POINTER;

    *ppcpc = nullptr;

    return E_NOTIMPL;
}
