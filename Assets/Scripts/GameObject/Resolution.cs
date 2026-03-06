using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Resolution
{
    public static void RotateToTarget(SoldierStructureBase lockTarget, WeaponBase weapon)
    {
        if(lockTarget == null || lockTarget.isDead || !weapon.enemiesInSightRange.Contains (lockTarget))
        {
            lockTarget = weapon.SerchEnemiesForDistance ();
        }
        if(lockTarget == null || weapon.enemiesInSightRange.Count == 0) return;
        Vector3 targetDir = lockTarget.transform.position - weapon.transform.position;
        Quaternion targetRot = Quaternion.LookRotation (targetDir);//ËÄÔª×ø±ê

        float angleDiff = Quaternion.Angle (weapon.transform.rotation, targetRot);
        //if(angleDiff < 3f) weapon.isAim = true;

        weapon.transform.rotation = Quaternion.RotateTowards (
        weapon.transform.rotation,
        targetRot,
        weapon.weaponTurnSpeed * Time.deltaTime);

    }
}