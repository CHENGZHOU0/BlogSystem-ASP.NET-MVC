﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.Models
{
    public class User:BaseEntity
    {
        [Required,StringLength(maximumLength:40),Column(TypeName = "varchar")]
        public string Email { get; set; }
        [Required,StringLength(maximumLength:30),Column(TypeName = "varchar")]
        public string Password { get; set; }
        [Required,StringLength(maximumLength:300),Column(TypeName = "varchar")]
        public string ImagePath { get; set; }
        /// <summary>
        /// 粉丝数量
        /// </summary>
        public int FansCount { get; set; }
        /// <summary>
        /// 关注数
        /// </summary>
        public int FocusCount { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string SiteName { get; set; }
    }
}
