namespace BootPOC.Util
{
    public interface IBootOptionsFactory
    {
        IBootOptions Create(string key);
    }
}