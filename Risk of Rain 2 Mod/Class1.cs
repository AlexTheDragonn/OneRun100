using BepInEx;
using RoR2;

namespace AlexTheDragon
{
    [BepInDependency("com.bepis.r2api")]
    //Change these
    [BepInPlugin("com.AlexTheDragon.TestMod", "My Mod's Title and if we see this exact name on Thunderstore we will deprecate your mod", "1.0.0")]
    public class TestMod : BaseUnityPlugin
    {
        public void Awake()
        {
        }
    }
}