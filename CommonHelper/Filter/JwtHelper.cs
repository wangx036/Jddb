using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CommonHelper.Filter
{
    public class JwtHelper
    {
        /// <summary>
        /// 颁发JWT字符串
        /// </summary>
        /// <param name="tokenModel"></param>
        /// <returns></returns>
        public static string IssueJWT(TokenModel tokenModel)
        {
            var jwtConfig = new JwtAuthConfigModel();
            //过期时间（分钟）
            double exp = 0;
            switch (tokenModel.TokenType)
            {
                case "Web":
                    exp = jwtConfig.WebExp;
                    break;
                case "App":
                    exp = jwtConfig.AppExp;
                    break;
                case "Wx":
                    exp = jwtConfig.WxExp;
                    break;
                case "Other":
                    exp = jwtConfig.OtherExp;
                    break;
            }
            var dateTime = DateTime.UtcNow;
            var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, tokenModel.Uid),
                    new Claim("UserName", tokenModel.UserName),//用户名
                    new Claim("UserType", tokenModel.UserType.ToString()),//用户类型
                    new Claim(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                    new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") , 
                    //这个就是过期时间，目前是过期100秒，可自定义，注意JWT有自己的缓冲过期时间
                    new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddMinutes(exp)).ToUnixTimeSeconds()}"),
                    new Claim(JwtRegisteredClaimNames.Iss,jwtConfig.Issuer),
                    new Claim(JwtRegisteredClaimNames.Aud,jwtConfig.Audience),
                    new Claim(ClaimTypes.Role,tokenModel.Role),
               };
            //秘钥
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.JWTSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: jwtConfig.Issuer,
                audience: jwtConfig.Audience,
                claims: claims,
                expires: dateTime.AddMinutes(exp),
                signingCredentials: creds);
            var jwtHandler = new JwtSecurityTokenHandler();
            var encodedJwt = jwtHandler.WriteToken(jwt);
            return encodedJwt;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        public static TokenModel SerializeJWT(string jwtStr)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);
            object role = new object();
            object userName = new object();
            object userType=new object();
            try
            {
                jwtToken.Payload.TryGetValue("UserName", out userName);
                jwtToken.Payload.TryGetValue("UserType", out userType);
                jwtToken.Payload.TryGetValue(ClaimTypes.Role, out role);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var tm = new TokenModel
            {
                Id = int.Parse(jwtToken.Id),
                Uid = jwtToken.Id,
                UserName = userName.ToString(),
                UserType = int.Parse(userType.ToString()),
                Role = role.ToString()
            };
            return tm;
        }
    }
    /// <summary>
    /// 令牌
    /// </summary>
    public class TokenModel
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }
        /// <summary>
        /// 身份
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 令牌类型
        /// </summary>
        public string TokenType { get; set; }
    }


    public class JwtAuthConfigModel
    {
        /// <summary>
        /// 
        /// </summary>
        public JwtAuthConfigModel()
        {
            try
            {
                JWTSecretKey = ConfigHelper.Configuration["JwtAuth:SecurityKey"];
                WebExp = double.Parse(ConfigHelper.Configuration["JwtAuth:WebExp"]);
                AppExp = double.Parse(ConfigHelper.Configuration["JwtAuth:AppExp"]);
                WxExp = double.Parse(ConfigHelper.Configuration["JwtAuth:WxExp"]);
                OtherExp = double.Parse(ConfigHelper.Configuration["JwtAuth:OtherExp"]);
                Issuer = ConfigHelper.Configuration["JwtAuth:Issuer"];
                Audience = ConfigHelper.Configuration["JwtAuth:Audience"];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string JWTSecretKey = "lyDqoSIQmyFcUhmmN4KBRGWWzm1ELC7owHVtStOu1YD7wYz";
        /// <summary>
        /// 
        /// </summary>
        public double WebExp = 12;
        /// <summary>
        /// 
        /// </summary>
        public double AppExp = 12;
        /// <summary>
        /// 
        /// </summary>
        public double WxExp = 12;
        /// <summary>
        /// 
        /// </summary>
        public double OtherExp = 12;

        public string Issuer = "jwt";

        public string Audience = "jwt";
    }
}