﻿namespace UBViews.Helpers;

using static Microsoft.Maui.ApplicationModel.Permissions;

internal class MyBluetoothPermission : BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new List<(string permission, bool isRuntime)>
        {
            ("android.permission.BLUETOOTH_SCAN", true),
            ("android.permission.BLUETOOTH_CONNECT", true)
        }.ToArray();
#endif
}
