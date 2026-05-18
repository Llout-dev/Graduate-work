public interface IHunger
{
    /// <summary>0 = сыт, 100 = очень голоден. Используется сенсором GOAP.</summary>
    int HungerLevel { get; }

    bool IsHungry();

    void Eat();
}