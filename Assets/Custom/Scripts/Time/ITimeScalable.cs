// ITimeScalable.cs
public interface ITimeScalable
{
    /// <summary> Asigna el factor local de tiempo (1 = normal, 0.2 = cámara lenta) </summary>
    void SetTimeScale(float scale);
}
