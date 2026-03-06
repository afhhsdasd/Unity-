using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

/// <summary>
/// 对于字符串 s 和 t，只有在 s = t + t + t + ... + t + t（t 自身连接 1 次或多次）时，我们才认定 “t 能除尽 s”。
///给定两个字符串 str1 和 str2 。返回 最长字符串 x，要求满足 x 能除尽 str1 且 x 能除尽 str2 。
/// </summary>
public class Solution1
{
    public string GcdOfStrings(string str1, string str2)
    {
        if(str1.Length < str2.Length)
        {
            string str = "";
            str = str1;
            str1 = str2;
            str2 = str;
        }
        if(str1 + str2 != str2 + str1)//减少逻辑的关键
            return "";
        int a = str1.Length;//求长度最大公约数
        int b = str2.Length;
        while(b != 0)
        {
            int c = a % b;
            a = b;
            b = c;
        }
        //
        StringBuilder sb = new StringBuilder ();
        for(int i = 0; i < a; i++)
        {
            sb.Append (str1[i]);
        }
        return sb.ToString ();
        
    }
}
/// <summary>
/// 有 n 个有糖果的孩子。给你一个数组 candies，其中 candies[i] 代表第 i 个孩子拥有的糖果数目，和一个整数 extraCandies 表示你所有的额外糖果的数量。
///返回一个长度为 n 的布尔数组 result，如果把所有的 extraCandies 给第 i 个孩子之后，他会拥有所有孩子中 最多 的糖果，那么 result[i] 为 true，否则为 false。
/// </summary>
public class Solution2
{
    public IList<bool> KidsWithCandies(int[] candies, int extraCandies)
    {
        int max = 0;
        List<bool> result = new List<bool>();
        foreach(int i in candies)
        {
            if(i > max)
                max = i;
        }
        for(int i = 0; i<candies.Length;i++)
        {
            if(candies[i]+ extraCandies >= max)
            {
                result.Add(true);
            }
            else
            {
                result.Add(false);
            }
        }
        return result;
    }
}
/// <summary>
/// 假设有一个很长的花坛，一部分地块种植了花，另一部分却没有。可是，花不能种植在相邻的地块上，它们会争夺水源，两者都会死去。
/// </summary>
public class Solution4
{
    public bool CanPlaceFlowers(int[] flowerbed, int n)
    {
        for(int i = 0; i < flowerbed.Length && n > 0; i++)
        {
            bool canPlant = flowerbed[i] == 0 && (i == 0 || flowerbed[i - 1] == 0) && (i == flowerbed.Length - 1 || flowerbed[i + 1] == 0);
            //短路运算符=>不会越界
            if(canPlant)
            {
                n--;
                flowerbed[i] = 1;
                i++;
            }
        }
        return n <= 0;
    }
}
/*寻路 / 图论：LeetCode 733. 图像渲染（Flood Fill，对应游戏消除类、地图染色）、200. 岛屿数量（DFS/BFS，对应游戏地图连通区域检测）→ 刷完能给你的 UI 框架加 “面板导航”“区域选择” 功能；
碰撞检测 / 区间问题：LeetCode 56. 合并区间（对应游戏技能范围、UI 控件重叠判断）、452. 用最少数量的箭引爆气球（区间贪心，对应游戏 AOE 伤害计算）；
随机 / 概率问题：LeetCode 384. 打乱数组（对应游戏道具随机掉落）、528. 按权重随机选择（对应游戏抽奖系统）→ 刷完能给你的登录面板加 “随机背景”“抽奖按钮” 功能；
资源管理 / 优化：LeetCode 146.LRU 缓存（对应 Unity 资源缓存池、UI 面板缓存）→ 刷完直接优化你的 UIMgr，减少面板频繁创建销毁的 GC。*/
/// <summary>
/// 有一幅以 m x n 的二维整数数组表示的图画 image ，其中 image[i][j] 表示该图画的像素值大小。你也被给予三个整数 sr ,  sc 和 color 。你应该从像素 image[sr][sc] 开始对图像进行上色 填充 。
/// </summary>
public class Solution733
{
    public int[][] FloodFill(int[][] image, int sr, int sc, int color)
    {
        int originalcolor = image[sr][sc];
        int outsideRange = image.Length;
        int insideRange = image[0].Length;
        if(image[sr][sc]== color)
        {
            return image;
        }
        Assist (sr, sc,color, outsideRange, insideRange, image, originalcolor);
        return image;
    }
    public void Assist(int i, int j,int col,int outside,int inside, int[][] image,int origin)
    {
        if(i < 0 || j < 0 || outside <= i||inside<=j)
        {
            return;
        }
        if(image[i][j] != origin)
        {
            return ;
        }
        image[i][j] = col;
        Assist (i - 1, j, col, outside, inside, image, origin);
        Assist (i + 1, j, col, outside, inside, image, origin);
        Assist(i,j-1, col, outside, inside, image, origin);
        Assist(i, j+1, col, outside, inside, image, origin);
    }
}
/// <summary>
/// 给你一个由 '1'（陆地）和 '0'（水）组成的的二维网格，请你计算网格中岛屿的数量。
///岛屿总是被水包围，并且每座岛屿只能由水平方向和/或竖直方向上相邻的陆地连接形成。
///此外，你可以假设该网格的四条边均被水包围。
/// </summary>
public class Solution200
{
    
    int island = 0;
    public struct Pos
    {
        public int X;
        public int Y;
        public Pos(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

    }
    public int NumIslands(char[][] grid)
    {
        if(grid == null || grid.Length == 0) return 0;
        int hmax = grid.Length;
        int vmax = grid[0].Length;
        //遍历数组，找到1启动bfs，改为0然后继续找
        for (int i = 0; i < hmax; i++)
        {
            for(int j = 0;  j < vmax; j++)
            {
                if(grid[i][j] == '0') continue;
                BFS(grid ,i,j, hmax, vmax);
                island++;
            }
        }
        return island;
    }
    public void BFS(char[][] grid,int h,int v,int hmax,int vmax)
    {
        Queue<Pos> queue = new Queue<Pos> ();
        Pos vector = new Pos (h,v);
        queue.Enqueue (vector);
        grid[vector.X][vector.Y] = '0';

        int[][] dirs = new int[][] { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 } };

        while(queue.Count > 0)
        {
            vector = queue.Dequeue ();
            int currX = vector.X;
            int currY = vector.Y;

            // 遍历4个方向，替代重复的4个if
            foreach(var dir in dirs)
            {
                int newX = currX + dir[0];
                int newY = currY + dir[1];
                if(newX >= 0 && newX < hmax && newY >= 0 && newY < vmax && grid[newX][newY] == '1')
                {
                    queue.Enqueue (new Pos (newX, newY));
                    grid[newX][newY] = '0';
                }
            }
        }
    }
}
/// <summary>
/// 以数组 intervals 表示若干个区间的集合，其中单个区间为 intervals[i] = [starti, endi] 。请你合并所有重叠的区间，并返回 一个不重叠的区间数组，该数组需恰好覆盖输入中的所有区间 。
/// </summary>
public class Solution56
{
    //Dictionary<int,int> numTotal = new Dictionary<int,int>();
    List<int[]> list = new List<int[]>();

    public int[][] Merge(int[][] intervals)
    {
        if(intervals == null || intervals.Length == 0 || intervals.Length == 1)
        {
            return intervals;
        }
        Array.Sort (intervals, (a, b) => a[0].CompareTo (b[0]));
        list.Add (intervals[0]);
        for(int i = 1; i < intervals.Length; i++)
        {
            if(intervals[i - 1][1] >= intervals[i][0])
            {
                list[i - 1][1] = Math.Max(intervals[i - 1][1], intervals[i][1]);
            }
            else
            {
                list.Add (intervals[i]);
            }
        }
        int[][] result = new int[list.Count][];
        for(int i=0; i<list.Count;i++)
        {
            result[i] = list[i];
        }
        return result;
    }
}
