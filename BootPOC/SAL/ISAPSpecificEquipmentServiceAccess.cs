namespace BootPOC.SAL
{
    public interface ISAPSpecificEquipmentServiceAccess
    {
        void RetrieveLiftList(bool forcePutInCache = false);
    }
}