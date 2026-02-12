using ColorBlockJam.Controllers;
using Zenject;

namespace ColorBlockJam.Installers
{
    public class AppInstaller : MonoInstaller<AppInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<SignalCenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InputController>().AsSingle().NonLazy();
        }
    }
}