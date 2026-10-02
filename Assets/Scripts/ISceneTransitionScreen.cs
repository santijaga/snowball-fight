// Implementations own the visual effect and report whether the screen was covered.
public interface ISceneTransitionScreen
{
    bool TryShowImmediate();
    void Hide();
}
