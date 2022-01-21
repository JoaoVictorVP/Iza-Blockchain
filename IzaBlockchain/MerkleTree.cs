using IzaBlockchain.Net;
using Mii.NET;
using System.Runtime.InteropServices;

namespace IzaBlockchain;

public class MerkleTree
{
    public readonly MerkleTreeNode Genesis;

    public MerkleTree(Ptr<Block> genesis)
    {
        Genesis = new MerkleTreeNode(genesis);
    }
}
public unsafe struct MerkleTreeNode : IDisposable
{
    public MerkleTreeNode* Parent;

    //public Block* Block;

    public Hash512 Hash;
    
    public NativeList<MerkleTreeNode> Branches;

    public void Dispose()
    {
        fixed(MerkleTreeNode* self = &this)
            NativeMemory.Free(self);

        foreach(var branch in Branches)
            branch.Dispose();
    }

    public int Deep => GetDeep(0);

    public int GetDeep(int from)
    {
        from++;

        int deep = from;
        foreach(var branch in Branches)
        {
            int ndeep = branch.GetDeep(deep);
            if(ndeep > deep)
                deep = ndeep;
        }
        return deep;
    }

    public bool IsConfirmedAndValid(int confirmations = 3)
    {
        if (IsValid(confirmations))
            return confirmations >= Deep;
        return false;
    }

    /// <summary>
    /// Checks if this node is valid in relaion with others
    /// </summary>
    /// <param name="node"></param>
    /// <param name="confirmations"></param>
    /// <returns></returns>
    public bool IsValid(int confirmations = 3)
    {
        if (confirmations <= 0)
            return true;

        var deep = MostDeepBranch();
        if(deep != null)
        {
            // Verify it this node is a valid parent of his branch
            if (!Hash.Equals(deep->Parent->Hash))
                return false;

            // Send this order to branch
            if (!deep->IsValid(confirmations - 1))
                return false;
        }
        return true;
    }

    public MerkleTreeNode* MostDeepBranch()
    {
        MerkleTreeNode* cur = null;
        int deep = 0;
        foreach(var branch in Branches)
        {
            int ndeep = branch.Deep;
            if(ndeep > deep)
            {
                cur = &branch;
                deep = ndeep;
            }
        }
        return cur;
    }

    public Ptr<MerkleTreeNode> QueryFor(BlockHash hash)
    {
        if (Hash.Equals(hash))
            return new Ptr<MerkleTreeNode>(ref this);
        int count = Branches.Count;
        for(int i = 0; i < count; i++)
        {
            ref var branch = ref Branches.FromIndex(i);
            var result = branch.QueryFor(hash);
            if (!result.IsNull)
                return result;
        }
        return default;
    }

    public MerkleTreeNode(Ptr<Block> block)
    {
        Parent = default;

        //Block = block.To;

        Hash = block.To->header.blockHash.ToHash512();
        
        Branches = new NativeList<MerkleTreeNode>(1);
    }
    public MerkleTreeNode(Ptr<Block> block, ref MerkleTreeNode parent)
    {
        fixed(MerkleTreeNode* parentPtr = &parent)
            Parent = parentPtr;

        Hash = block.To->header.blockHash.ToHash512();

        Branches = new NativeList<MerkleTreeNode>(1);
    }
}
