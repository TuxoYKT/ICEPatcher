﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GlobalPatcher.Localization {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GlobalPatcher.Localization.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Browse.
        /// </summary>
        internal static string button_browse {
            get {
                return ResourceManager.GetString("button_browse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Exit.
        /// </summary>
        internal static string button_done {
            get {
                return ResourceManager.GetString("button_done", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Patch.
        /// </summary>
        internal static string button_patch {
            get {
                return ResourceManager.GetString("button_patch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Not a pso2_bin folder..
        /// </summary>
        internal static string error_notpso2bin {
            get {
                return ResourceManager.GetString("error_notpso2bin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap globalpatcher_logo {
            get {
                object obj = ResourceManager.GetObject("globalpatcher_logo", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Patch complete. You can now close the program..
        /// </summary>
        internal static string status_complete {
            get {
                return ResourceManager.GetString("status_complete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Downloading patches..
        /// </summary>
        internal static string status_downloading {
            get {
                return ResourceManager.GetString("status_downloading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Exporting patches.
        /// </summary>
        internal static string status_exporting {
            get {
                return ResourceManager.GetString("status_exporting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Applying patches.
        /// </summary>
        internal static string status_patching {
            get {
                return ResourceManager.GetString("status_patching", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Something went wrong. Try again..
        /// </summary>
        internal static string status_patchingError {
            get {
                return ResourceManager.GetString("status_patchingError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на pso2_bin folder not set..
        /// </summary>
        internal static string status_pso2binNotSet {
            get {
                return ResourceManager.GetString("status_pso2binNotSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на pso2_bin folder is set..
        /// </summary>
        internal static string status_pso2binSet {
            get {
                return ResourceManager.GetString("status_pso2binSet", resourceCulture);
            }
        }
    }
}
