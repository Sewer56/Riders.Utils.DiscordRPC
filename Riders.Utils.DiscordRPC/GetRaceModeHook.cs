using System;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interop;
using Sewer56.SonicRiders.Structures.Enums;

namespace Riders.Utils.DiscordRPC
{
    public unsafe class GetRaceModeHook
    {
        public RaceMode RaceMode => (RaceMode) _pinnable.Value;

        private Pinnable<int> _pinnable = new Pinnable<int>((int) RaceMode.FreeRace);
        private IAsmHook _getRaceModeHook = null;
        private IReloadedHooks _hooks;

        public GetRaceModeHook(IReloadedHooks hooks)
        {
            _hooks = hooks;
            var hook = new string[]
            {
                "use32",
                // Callee Register Backup
                $"mov [{(IntPtr)_pinnable.Pointer}], eax"
            };

            _getRaceModeHook = hooks.CreateAsmHook(hook, 0x0046C116, AsmHookBehaviour.ExecuteAfter).Activate();
        }

        public void Resume()  => _getRaceModeHook.Enable();
        public void Suspend() => _getRaceModeHook.Disable();
    }
}
