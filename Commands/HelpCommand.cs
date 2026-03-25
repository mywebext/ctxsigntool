using CtxSignTool.Contracts;
using CtxSignTool.Help;
using CtxSignTool.Routing;

namespace CtxSignTool.Commands;
/*
 * CtxSignTool design laws
Law 1 — Primary operation law

--sign and --verify are the two primary operation pipes.

If the end goal is signing, route through --sign.
If the end goal is verification, route through --verify.

Never create alternate sign/verify command families for work that belongs inside those pipes.

Law 2 — Target qualifier law

Flags such as --manifest describe what is being acted on, not a new operation.

Examples:

--sign --manifest = sign a manifest

--verify --manifest = verify a manifest

Do not replace this with new verbs like --verify-manifest.

Law 3 — End-goal law

If the end goal is the same, the pipe must remain the same.

Equivalent outcomes must not be split into separate command families just because the internal logic differs.

Internal implementation may branch.
External command structure must stay unified.

Law 4 — No verb multiplication law

Do not introduce new --sign-* or --verify-* forms.

All sign behavior stays under --sign.
All verify behavior stays under --verify.

Law 5 — Manifest scope law

--partial, --detail, --details, and --detailed apply only to manifest-based verification.

They do not apply to single-file verification unless the library is explicitly extended later.

Law 6 — Single-file simplicity law

Single-file verification is pass/fail only.

A file either:

exists or not

verifies or not

matches the pin or not

Do not force manifest-style reporting onto single-file verification.

Law 7 — Manifest detail law

Detailed reporting exists because manifest verification evaluates a set.

Manifest verification may classify files into:

passed

missing

failed

unreadable

Detailed result handling belongs to this domain.

Law 8 — Strict default law

Bare --verify --manifest is strict unless another valid modifier explicitly changes policy.

Strict means:

missing file = fail

failed hash = fail

unreadable file = fail

Law 9 — Partial policy law

--partial changes only manifest file-presence policy.

In partial mode:

missing file = allowed

failed hash = fail

unreadable file = fail

Manifest signature/authentication failure still fails.

Law 10 — Reporting separation law

Policy flags and reporting flags are separate concepts.

--partial changes verification policy

--details changes reporting depth

Do not merge these into one flag.

Law 11 — Backward compatibility law

New flags may extend behavior, but they must not silently redefine the meaning of existing bare commands.

Existing users of --verify must keep the same default behavior.

Law 12 — Routing law

Routing is determined in this order:

primary operation (--sign or --verify)

target qualifier (--manifest, file inputs, etc.)

policy modifiers (--partial)

reporting modifiers (--details)
 * */
/// <summary>
/// Provides functionality for executing help commands and displaying help information based on the specified command
/// context and help topic.
/// </summary>
/// <remarks>Use this class to present help information to users within the application. The help command can be
/// tailored to different topics and contexts, allowing for flexible and context-sensitive assistance. All members are
/// static and intended for direct invocation without instantiation.</remarks>
public static class HelpCommand
{
    /// <summary>
    /// Executes the help command and displays help information for the specified target using the provided command
    /// context.
    /// </summary>
    /// <remarks>Use this method to present help information to users based on the current command context and
    /// selected help topic. Ensure that the context is valid before calling this method.</remarks>
    /// <param name="context">The context in which the command is executed. Must be properly initialized to provide relevant information for
    /// help display.</param>
    /// <param name="target">Specifies the help topic to display. Defaults to <see cref="HelpTarget.General"/> if not provided.</param>
    /// <param name="code">Indicates the return code for the operation. Defaults to <see cref="ReturnCodes.Ok"/> if not specified.</param>
    /// <returns>An integer representing the result of the help display operation. Typically, <see cref="ReturnCodes.Ok"/>
    /// indicates success.</returns>
    public static int Execute(CommandContext context, HelpTarget target = HelpTarget.General, ReturnCodes code = ReturnCodes.Ok)
    {
        return HelpSystem.Print(target, context, code);
    }
}
