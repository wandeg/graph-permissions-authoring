using Kibali;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthoringPrototype;

public static class PermissionsMerger
{
    public static PermissionsDocument MergePermissionsDocument(PermissionsDocument existingDocument, PermissionsDocument newDocument)
    {
        if (existingDocument == null)
        {
            return newDocument;
        }
        var result = new PermissionsDocument();
 
        foreach (var permission in existingDocument.Permissions)
        {
            result.Permissions.Add(permission.Key, permission.Value);
        }
 
        foreach (var permission in newDocument.Permissions)
        {
            if (result.Permissions.TryGetValue(permission.Key, out var existingPermission))
            {
                foreach (var scheme in permission.Value.Schemes)
                {
                    if (existingPermission.Schemes.TryGetValue(scheme.Key, out var existingScheme))
                    {
                        continue;
                    }
                    else
                    {
                        existingPermission.Schemes.Add(scheme.Key, scheme.Value);
                    }
                }
                foreach (var pathSet in permission.Value.PathSets)
                {
                    var existingPathset = existingPermission.PathSets.Where(x => new PathsetEqualityComparer().Equals(x, pathSet)).FirstOrDefault();
                    if (existingPathset != null)
                    {
                        // merge paths
                        foreach (var path in pathSet.Paths)
                        {
                            if (!existingPathset.Paths.ContainsKey(path.Key))
                            {
                                existingPathset.Paths.Add(path.Key, path.Value);
                            }
                        }
                    }
                    else
                    {
                        existingPermission.PathSets.Add(pathSet);
                    }
                }
            }
            else
            {
                result.Permissions.Add(permission.Key, permission.Value);
            }
        }
        return result;
    }
}
