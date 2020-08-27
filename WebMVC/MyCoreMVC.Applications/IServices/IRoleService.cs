﻿using MyCoreMvc.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyCoreMVC.Applications.IServices
{
    /// <summary>
    /// 角色业务
    /// </summary>
    public interface IRoleService : IBaseService
    {
        IQueryable<Role> GerAll();

        Role Get(int id);

        Role Add(Role user);

        Role Update(Role user);
    }
}
