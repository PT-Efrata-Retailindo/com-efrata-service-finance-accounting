﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.Efrata.Service.Finance.Accounting.Lib.Utilities.BaseInterface
{
    public interface IBaseService<TModel>
    {

        ReadResponse<TModel> Read(int page, int size, string order, List<string> select, string keyword, string filter);
        Task<int> CreateAsync(TModel model);
        Task<TModel> ReadByIdAsync(int id);
        Task<int> UpdateAsync(int id, TModel model);
        Task<int> DeleteAsync(int id);
    }
}
