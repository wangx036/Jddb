using System;

namespace CommonHelper
{
    // <summary>
    /// 用户登陆信息提供者。
    /// </summary>
    public class OperatorProvider
    {
        /// <summary>
        /// Session/Cookie键。
        /// </summary>
        private string LOGIN_USER_KEY = ConfigHelper.Configuration["Operator:LoginKey"];
        /// <summary>
        /// 从配置文件读取登陆提供者模式(Session/Cookie)。
        /// </summary>
        private string LoginProvider = ConfigHelper.Configuration["Operator:LoginProvider"];

        /// <summary>
        /// 从配置文件读取登陆用户信息保存时间。
        /// </summary>
        private int LoginTimeout = int.Parse(ConfigHelper.Configuration["Operator:LoginTimeout"]);



        private OperatorProvider() { }

        static OperatorProvider() { }

        //使用内部类+静态构造函数实现延迟初始化。
        class Nested
        {
            static Nested() { }
            internal static readonly OperatorProvider instance = new OperatorProvider();
        }
        /// <summary>
        /// 在大多数情况下，静态初始化是在.NET中实现Singleton的首选方法。
        /// </summary>
        public static OperatorProvider Instance
        {
            get
            {
                return Nested.instance;
            }
        }


        /// <summary>
        /// 从Session/Cookie获取或设置用户操作模型。
        /// </summary>
        /// <returns></returns>
        public Operator Current
        {
            get
            {
                Operator operatorModel = new Operator();
                if (LoginProvider == "Cookie")
                {
                    operatorModel = CookieHelper.Get(LOGIN_USER_KEY).DESDecrypt().ToObject<Operator>();
                }
                else
                {
                    operatorModel = SessionHelper.Get(LOGIN_USER_KEY).DESDecrypt().ToObject<Operator>();
                }
                return operatorModel;
            }
            set
            {
                if (LoginProvider == "Cookie")
                {
                    CookieHelper.Set(LOGIN_USER_KEY, value.ToJson().DESEncrypt(), LoginTimeout);
                }
                else
                {
                    SessionHelper.Set(LOGIN_USER_KEY, value.ToJson().DESEncrypt());
                }
            }
        }

        /// <summary>
        /// 从Session/Cookie删除用户操作模型。
        /// </summary>
        public void Remove()
        {
            if (LoginProvider == "Cookie")
            {
                CookieHelper.Remove(LOGIN_USER_KEY);
            }
            else
            {
                SessionHelper.Remove(LOGIN_USER_KEY);
            }
        }

    }

    /// <summary>
    /// 操作模型，保存登陆用户必要信息。
    /// </summary>
    public class Operator
    {
        /// <summary>
        /// 用户id（未登录为0）
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 微信用户的唯一标识
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string HeadimgUrl { get; set; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }
        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LoginTime { get; set; }
    }
}