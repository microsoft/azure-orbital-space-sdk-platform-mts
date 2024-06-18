namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService;
public static class Models {
    public class APP_CONFIG : Core.APP_CONFIG {
        [Flags]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum PluginPermissions {
            NONE = 0,
            SENSORS_AVAILABLE_REQUEST = 1 << 0,
            SENSORS_AVAILABLE_RESPONSE = 1 << 1,
            TASKING_PRECHECK_REQUEST = 1 << 2,
            TASKING_PRECHECK_RESPONSE = 1 << 3,
            TASKING_REQUEST = 1 << 4,
            TASKING_RESPONSE = 1 << 5,
            SENSOR_DATA = 1 << 6,
            ALL = SENSORS_AVAILABLE_REQUEST | SENSORS_AVAILABLE_RESPONSE | TASKING_PRECHECK_REQUEST | TASKING_PRECHECK_RESPONSE | TASKING_REQUEST | TASKING_RESPONSE | SENSOR_DATA
        }

        public class PLUG_IN : Core.Models.PLUG_IN {
            [JsonConverter(typeof(JsonStringEnumConverter))]

            public PluginPermissions CALCULATED_PLUGIN_PERMISSIONS {
                get {
                    PluginPermissions result;
                    System.Enum.TryParse(PLUGIN_PERMISSIONS, out result);
                    return result;
                }
            }

            public PLUG_IN() {
                PLUGIN_PERMISSIONS = "";
                PROCESSING_ORDER = 100;
            }
        }

        public int MESSAGE_RESPONSE_TIMEOUT_MS { get; set; }

        public APP_CONFIG() : base() {
            MESSAGE_RESPONSE_TIMEOUT_MS = int.Parse(Core.GetConfigSetting("messageresponsetimeoutms").Result);
        }
    }
}