public interface IParryable
{
    bool IsParried { get; }
    void Parry();
    void Remove();
}