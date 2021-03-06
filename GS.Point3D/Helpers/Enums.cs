﻿/* Copyright(C) 2021  Rob Morgan (robert.morgan.e@gmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
namespace GS.Point3D.Helpers
{
    #region Enums

    public enum Model3DType
    {
        Default = 0,
        Reflector = 1,
        Refractor = 2,
        SchmidtCassegrain = 3,
        RitcheyChretien = 4,
        RitcheyChretienTruss = 5
    }

    public enum SOP
    {
        pierUnknown = -1,
        pierEast = 0,
        pierWest = 1
    }


    /// <summary>
    /// Side of Pier
    /// </summary>
    public enum Sop
    {
        None = -1,
        East = 0,
        West = 1
    }

    public enum AlignMode
    {
        algUnknown,
        algAltAz,
        algPolar,
        algGermanPolar
    }

    #endregion
}
