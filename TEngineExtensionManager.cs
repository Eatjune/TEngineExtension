using Cysharp.Threading.Tasks;

namespace GameLogic {
	public class TEngineExtensionManager : Singleton<TEngineExtensionManager> {
		public new async UniTask Active() {
			await ConfigManager.Instance.PreLoadConfigData();
		}

		protected override void OnInit() {
			base.OnInit();
		}
	}
}
