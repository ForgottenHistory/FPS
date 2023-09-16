public interface IInitialize
{
    public bool isActive { get; set; }
    public void Initialize();
    public void Deinitialize();
}