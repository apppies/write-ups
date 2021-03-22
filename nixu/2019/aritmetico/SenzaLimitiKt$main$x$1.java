package p000;

import java.math.BigDecimal;
import kotlin.Metadata;
import kotlin.jvm.functions.Function1;
import kotlin.jvm.internal.Intrinsics;
import kotlin.jvm.internal.Lambda;
import org.jetbrains.annotations.NotNull;

final class SenzaLimitiKt$main$x$1 extends Lambda implements Function1<BigDecimal, BigDecimal> {
    public static final SenzaLimitiKt$main$x$1 INSTANCE = new SenzaLimitiKt$main$x$1();

    SenzaLimitiKt$main$x$1() {
        super(1);
    }

    public final BigDecimal invoke(@NotNull BigDecimal it) {
        Intrinsics.checkParameterIsNotNull(it, "it");
        BigDecimal add = it.add(SenzaLimitiKt.getUNO());
        Intrinsics.checkExpressionValueIsNotNull(add, "this.add(other)");
        return add;
    }
}
