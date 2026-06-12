using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Application.Mapper
{
    public interface IFixtureMapper
    {
        FixtureDaily2Dto Map(HttpRequest request);
    }
}
