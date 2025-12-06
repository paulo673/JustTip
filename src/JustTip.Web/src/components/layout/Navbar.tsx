import { cn } from '../../lib/utils'

interface NavbarProps {
  activePage?: 'roster' | 'tips'
}

function UserAvatar() {
  return (
    <div className="w-9 h-9 rounded-full bg-slate-200 flex items-center justify-center text-sm font-medium text-slate-600 cursor-pointer hover:bg-slate-300 transition-colors">
      JD
    </div>
  )
}

export function Navbar({ activePage = 'roster' }: NavbarProps) {
  return (
    <nav className="h-16 border-b border-slate-200 bg-white">
      <div className="max-w-7xl mx-auto px-6 h-full flex items-center justify-between">
        <div className="flex items-center gap-10">
          <span className="text-xl font-bold text-slate-900 tracking-tight">
            JustTip
          </span>

          <div className="flex items-center gap-1">
            <a
              href="#roster"
              className={cn(
                "px-4 py-2 text-sm font-medium rounded-md transition-colors",
                activePage === 'roster'
                  ? "text-slate-900 bg-slate-100"
                  : "text-slate-600 hover:text-slate-900 hover:bg-slate-50"
              )}
            >
              Roster
            </a>
            <a
              href="#tips"
              className={cn(
                "px-4 py-2 text-sm font-medium rounded-md transition-colors",
                activePage === 'tips'
                  ? "text-slate-900 bg-slate-100"
                  : "text-slate-600 hover:text-slate-900 hover:bg-slate-50"
              )}
            >
              Tips
            </a>
          </div>
        </div>

        <UserAvatar />
      </div>
    </nav>
  )
}
